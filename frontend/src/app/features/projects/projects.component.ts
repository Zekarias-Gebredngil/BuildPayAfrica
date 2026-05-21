import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

export type ProjectDto = {
  id: string;
  customerUserId?: string;
  contractorUserId?: string | null;
  name: string;
  budget: number;
  status: string;
  createdAt?: string;
};

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './projects.component.html'
})
export class ProjectsComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  projects: ProjectDto[] = [];
  loading  = true;
  error: string | null = null;

  creating    = false;
  createError: string | null = null;
  showForm    = false;

  form = this.fb.group({
    name:   ['', [Validators.required, Validators.minLength(3)]],
    budget: [0,  [Validators.required, Validators.min(1)]]
  });

  constructor() { this.load(); }

  get isCustomer() { return this.auth.roles.includes('Customer'); }
  get isContractor() { return this.auth.roles.includes('Contractor'); }
  get isAdmin() { return this.auth.roles.includes('Admin'); }

  load() {
    this.loading = true;
    this.error = null;

    const roles = this.auth.roles;
    const url =
      roles.includes('Admin') ? `/api/projects/all` :
      roles.includes('Contractor') ? `/api/projects/assigned` :
      `/api/projects/my`;

    this.http.get<ProjectDto[]>(url).subscribe({
      next:  (res) => { this.projects = res; this.loading = false; },
      error: ()    => { this.loading = false; this.error = 'Failed to load projects'; }
    });
  }

  create() {
    if (!this.isCustomer) return;
    if (this.form.invalid) return;
    const { name, budget } = this.form.getRawValue();
    this.creating = true;
    this.createError = null;
    this.http.post(`/api/projects`, { name, budget }).subscribe({
      next: () => {
        this.creating = false;
        this.showForm = false;
        this.form.reset({ name: '', budget: 0 });
        this.load();
      },
      error: (e) => {
        this.creating = false;
        this.createError = e?.error?.message ?? 'Failed to create project';
      }
    });
  }

  statusColor(status: string): string {
    const map: Record<string, string> = {
      Active:    'bp-badge-blue',
      Completed: 'bp-badge-green',
      Pending:   'bp-badge-yellow',
      Cancelled: 'bp-badge-red'
    };
    return map[status] ?? 'bp-badge-gray';
  }
}

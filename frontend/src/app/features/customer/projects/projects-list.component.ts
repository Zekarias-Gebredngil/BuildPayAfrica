import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProjectDto } from './project.models';

@Component({
  selector: 'app-customer-projects',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './projects-list.component.html'
})
export class CustomerProjectsComponent {
  private readonly http   = inject(HttpClient);
  private readonly fb     = inject(FormBuilder);
  private readonly router = inject(Router);

  projects = signal<ProjectDto[]>([]);
  loading  = signal(true);
  error    = signal<string | null>(null);

  showCreateForm = signal(false);
  creating       = signal(false);
  createError    = signal<string | null>(null);

  // Delete confirmation
  deletingId = signal<string | null>(null);

  form = this.fb.group({
    name:   ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    budget: [null as number | null, [Validators.required, Validators.min(1)]]
  });

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<ProjectDto[]>('/api/projects/my').subscribe({
      next:  (res) => { this.projects.set(res); this.loading.set(false); },
      error: (e)   => { this.loading.set(false); this.error.set(e?.error?.message ?? 'Failed to load projects'); }
    });
  }

  create() {
    if (this.form.invalid) return;
    const { name, budget } = this.form.getRawValue();
    this.creating.set(true);
    this.createError.set(null);

    this.http.post<{ projectId: string }>('/api/projects', { name, budget }).subscribe({
      next: () => {
        this.creating.set(false);
        this.showCreateForm.set(false);
        this.form.reset();
        this.load();
      },
      error: (e) => {
        this.creating.set(false);
        this.createError.set(e?.error?.message ?? 'Failed to create project');
      }
    });
  }

  openProject(id: string) {
    this.router.navigate(['/features/customer/projects', id]);
  }

  confirmDelete(id: string) { this.deletingId.set(id); }
  cancelDelete()            { this.deletingId.set(null); }

  deleteProject(id: string) {
    this.http.delete(`/api/projects/${id}`).subscribe({
      next:  () => { this.deletingId.set(null); this.load(); },
      error: (e) => { this.deletingId.set(null); this.error.set(e?.error?.message ?? 'Delete failed'); }
    });
  }

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      Draft:     'bp-badge-gray',
      Active:    'bp-badge-blue',
      Completed: 'bp-badge-green',
      Cancelled: 'bp-badge-red'
    };
    return map[status] ?? 'bp-badge-gray';
  }

  statusIcon(status: string): string {
    const map: Record<string, string> = {
      Draft: '📝', Active: '🔨', Completed: '✅', Cancelled: '✕'
    };
    return map[status] ?? '📋';
  }
}

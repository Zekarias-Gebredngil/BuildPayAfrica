import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

type ProjectDto = {
  id: string;
  customerId: string;
  contractorId?: string | null;
  name: string;
  budget: number;
  status: string;
};

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <div class="flex flex-col items-start justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h2 class="text-xl font-semibold">Projects</h2>
            <p class="mt-1 text-sm text-slate-600">Create projects and manage milestones, expenses, and escrow.</p>
          </div>
          <a routerLink="/contractor/verification" class="rounded-md border px-4 py-2 text-sm hover:bg-slate-100">
            Contractor verification
          </a>
        </div>
      </div>

      <div class="rounded-2xl border bg-white p-6">
        <h3 class="text-sm font-semibold">Create project</h3>
        <form class="mt-3 grid gap-3 sm:grid-cols-3" [formGroup]="form" (ngSubmit)="create()">
          <input class="rounded-md border px-3 py-2 text-sm sm:col-span-2" placeholder="Project name" formControlName="name" />
          <input class="rounded-md border px-3 py-2 text-sm" placeholder="Budget" type="number" formControlName="budget" />
          <div class="sm:col-span-3">
            <button class="rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60" [disabled]="form.invalid || creating">
              {{ creating ? 'Creating…' : 'Create' }}
            </button>
          </div>
        </form>
        <div *ngIf="createError" class="mt-3 rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ createError }}</div>
      </div>

      <div class="rounded-2xl border bg-white p-6">
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-semibold">My projects</h3>
          <button class="rounded-md border px-3 py-1.5 text-sm hover:bg-slate-100" (click)="load()">Refresh</button>
        </div>

        <div *ngIf="loading" class="mt-3 text-sm text-slate-600">Loading…</div>
        <div *ngIf="error" class="mt-3 rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>

        <div class="mt-3 grid gap-3">
          <a *ngFor="let p of projects" [routerLink]="['/projects', p.id]" class="rounded-xl border p-4 hover:bg-slate-50">
            <div class="flex items-center justify-between gap-4">
              <div>
                <div class="text-sm font-semibold">{{ p.name }}</div>
                <div class="mt-1 text-xs text-slate-500">Budget: {{ p.budget | number : '1.0-2' }} • Status: {{ p.status }}</div>
              </div>
              <div class="text-xs text-slate-500">{{ p.contractorId ? 'Contractor assigned' : 'No contractor' }}</div>
            </div>
          </a>
        </div>
      </div>
    </div>
  `
})
export class ProjectsComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  projects: ProjectDto[] = [];
  loading = true;
  error: string | null = null;

  creating = false;
  createError: string | null = null;

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    budget: [0, [Validators.required, Validators.min(0)]]
  });

  constructor() {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = null;
    this.http.get<ProjectDto[]>(`/api/projects/my`).subscribe({
      next: (res) => {
        this.projects = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load projects';
      }
    });
  }

  create() {
    if (this.form.invalid) return;
    const { name, budget } = this.form.getRawValue();

    this.creating = true;
    this.createError = null;
    this.http.post(`/api/projects`, { name, budget }).subscribe({
      next: () => {
        this.creating = false;
        this.form.reset({ name: '', budget: 0 });
        this.load();
      },
      error: (e) => {
        this.creating = false;
        this.createError = e?.error?.message ?? 'Failed to create project';
      }
    });
  }
}


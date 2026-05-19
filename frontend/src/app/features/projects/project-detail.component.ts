import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';

type ProjectDto = { id: string; name: string; budget: number; status: string; contractorId?: string | null };
type MilestoneDto = { id: string; title: string; amount: number; status: string; dueDate: string };
type ExpenseDto = { id: string; title: string; amount: number; receiptUrl?: string | null; createdAt: string };

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="grid gap-6" *ngIf="projectId">
      <div class="rounded-2xl border bg-white p-6">
        <div class="flex flex-col items-start justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <div class="text-xs text-slate-500">Project</div>
            <h2 class="text-xl font-semibold">{{ project?.name }}</h2>
            <div class="mt-1 text-sm text-slate-600">Budget: {{ project?.budget | number : '1.0-2' }} • Status: {{ project?.status }}</div>
          </div>
          <div class="rounded-md border bg-slate-50 px-3 py-2 text-sm">
            Escrow balance: <span class="font-semibold">{{ balance | number : '1.0-2' }}</span>
          </div>
        </div>

        <form class="mt-4 flex flex-wrap items-end gap-2" [formGroup]="fundForm" (ngSubmit)="fundEscrow()">
          <label class="grid gap-1 text-sm">
            <span>Fund escrow</span>
            <input class="w-40 rounded-md border px-3 py-2" type="number" min="1" formControlName="amount" />
          </label>
          <button class="rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60" [disabled]="fundForm.invalid || funding">
            {{ funding ? 'Funding…' : 'Fund' }}
          </button>
        </form>
      </div>

      <div class="grid gap-4 md:grid-cols-2">
        <div class="rounded-2xl border bg-white p-6">
          <h3 class="text-sm font-semibold">Milestones</h3>

          <form class="mt-3 grid gap-2" [formGroup]="milestoneForm" (ngSubmit)="addMilestone()">
            <input class="rounded-md border px-3 py-2 text-sm" placeholder="Title" formControlName="title" />
            <div class="grid grid-cols-2 gap-2">
              <input class="rounded-md border px-3 py-2 text-sm" placeholder="Amount" type="number" min="1" formControlName="amount" />
              <input class="rounded-md border px-3 py-2 text-sm" type="date" formControlName="dueDate" />
            </div>
            <button class="rounded-md border px-3 py-2 text-sm hover:bg-slate-100 disabled:opacity-60" [disabled]="milestoneForm.invalid || addingMilestone">
              {{ addingMilestone ? 'Adding…' : 'Add milestone' }}
            </button>
          </form>

          <div class="mt-4 grid gap-2">
            <div *ngFor="let m of milestones" class="rounded-xl border p-3">
              <div class="flex items-center justify-between gap-3">
                <div>
                  <div class="text-sm font-semibold">{{ m.title }}</div>
                  <div class="mt-1 text-xs text-slate-500">Amount: {{ m.amount | number : '1.0-2' }} • Due: {{ m.dueDate | date }}</div>
                </div>
                <div class="text-xs text-slate-600">Status: {{ m.status }}</div>
              </div>

              <div class="mt-3 flex flex-wrap gap-2">
                <button
                  *ngIf="auth.roles.includes('Contractor')"
                  class="rounded-md border px-2 py-1 text-xs hover:bg-slate-100"
                  (click)="requestRelease(m.id)"
                >
                  Request release
                </button>
                <button
                  *ngIf="auth.roles.includes('Customer')"
                  class="rounded-md bg-slate-900 px-2 py-1 text-xs text-white hover:bg-slate-800"
                  (click)="approveRelease(m.id)"
                >
                  Approve + pay
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="rounded-2xl border bg-white p-6">
          <h3 class="text-sm font-semibold">Expenses</h3>

          <form class="mt-3 grid gap-2" [formGroup]="expenseForm" (ngSubmit)="addExpense()">
            <input class="rounded-md border px-3 py-2 text-sm" placeholder="Title" formControlName="title" />
            <input class="rounded-md border px-3 py-2 text-sm" placeholder="Amount" type="number" min="1" formControlName="amount" />
            <input class="rounded-md border px-3 py-2 text-sm" placeholder="Receipt URL (optional)" formControlName="receiptUrl" />
            <button class="rounded-md border px-3 py-2 text-sm hover:bg-slate-100 disabled:opacity-60" [disabled]="expenseForm.invalid || addingExpense">
              {{ addingExpense ? 'Adding…' : 'Add expense' }}
            </button>
          </form>

          <div class="mt-4 grid gap-2">
            <div *ngFor="let e of expenses" class="rounded-xl border p-3">
              <div class="flex items-center justify-between gap-3">
                <div>
                  <div class="text-sm font-semibold">{{ e.title }}</div>
                  <div class="mt-1 text-xs text-slate-500">{{ e.createdAt | date : 'medium' }}</div>
                </div>
                <div class="text-sm font-semibold">{{ e.amount | number : '1.0-2' }}</div>
              </div>
              <a *ngIf="e.receiptUrl" class="mt-2 block text-xs underline" [href]="e.receiptUrl" target="_blank" rel="noopener">Receipt</a>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
    </div>
  `
})
export class ProjectDetailComponent {
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);

  projectId: string | null = null;
  project: ProjectDto | null = null;
  milestones: MilestoneDto[] = [];
  expenses: ExpenseDto[] = [];
  balance = 0;
  error: string | null = null;

  funding = false;
  addingMilestone = false;
  addingExpense = false;

  fundForm = this.fb.group({ amount: [1000, [Validators.required, Validators.min(1)]] });
  milestoneForm = this.fb.group({
    title: ['', [Validators.required]],
    amount: [0, [Validators.required, Validators.min(1)]],
    dueDate: ['', [Validators.required]]
  });
  expenseForm = this.fb.group({
    title: ['', [Validators.required]],
    amount: [0, [Validators.required, Validators.min(1)]],
    receiptUrl: ['']
  });

  constructor() {
    this.projectId = this.route.snapshot.paramMap.get('id');
    this.refresh();
  }

  refresh() {
    if (!this.projectId) return;
    this.error = null;

    this.http.get<ProjectDto>(`/api/projects/${this.projectId}`).subscribe({
      next: (p) => (this.project = p),
      error: () => (this.error = 'Failed to load project')
    });

    this.http.get<MilestoneDto[]>(`/api/projects/${this.projectId}/milestones`).subscribe({
      next: (m) => (this.milestones = m),
      error: () => (this.error = 'Failed to load milestones')
    });

    this.http.get<ExpenseDto[]>(`/api/projects/${this.projectId}/expenses`).subscribe({
      next: (e) => (this.expenses = e),
      error: () => (this.error = 'Failed to load expenses')
    });

    this.http.get<{ balance: number }>(`/api/payments/projects/${this.projectId}/balance`).subscribe({
      next: (b) => (this.balance = b.balance),
      error: () => (this.error = 'Failed to load escrow balance')
    });
  }

  fundEscrow() {
    if (!this.projectId || this.fundForm.invalid) return;
    this.funding = true;
    const amount = Number(this.fundForm.getRawValue().amount);
    this.http.post(`/api/payments/projects/${this.projectId}/fund`, { amount }).subscribe({
      next: () => {
        this.funding = false;
        this.refresh();
      },
      error: (e) => {
        this.funding = false;
        this.error = e?.error?.message ?? 'Funding failed';
      }
    });
  }

  addMilestone() {
    if (!this.projectId || this.milestoneForm.invalid) return;
    this.addingMilestone = true;
    const v = this.milestoneForm.getRawValue();
    this.http
      .post(`/api/projects/${this.projectId}/milestones`, { title: v.title, amount: v.amount, dueDate: new Date(v.dueDate!).toISOString() })
      .subscribe({
        next: () => {
          this.addingMilestone = false;
          this.milestoneForm.reset({ title: '', amount: 0, dueDate: '' });
          this.refresh();
        },
        error: (e) => {
          this.addingMilestone = false;
          this.error = e?.error?.message ?? 'Add milestone failed';
        }
      });
  }

  addExpense() {
    if (!this.projectId || this.expenseForm.invalid) return;
    this.addingExpense = true;
    const v = this.expenseForm.getRawValue();
    this.http.post(`/api/projects/${this.projectId}/expenses`, v).subscribe({
      next: () => {
        this.addingExpense = false;
        this.expenseForm.reset({ title: '', amount: 0, receiptUrl: '' });
        this.refresh();
      },
      error: (e) => {
        this.addingExpense = false;
        this.error = e?.error?.message ?? 'Add expense failed';
      }
    });
  }

  requestRelease(milestoneId: string) {
    this.http.post(`/api/payments/milestones/${milestoneId}/request-release`, {}).subscribe({
      next: () => this.refresh(),
      error: (e) => (this.error = e?.error?.message ?? 'Request failed')
    });
  }

  approveRelease(milestoneId: string) {
    this.http.post(`/api/payments/milestones/${milestoneId}/approve-release`, {}).subscribe({
      next: () => this.refresh(),
      error: (e) => (this.error = e?.error?.message ?? 'Approval failed')
    });
  }
}


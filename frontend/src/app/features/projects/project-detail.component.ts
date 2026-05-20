import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';

export type ProjectDto    = { id: string; name: string; budget: number; status: string; contractorId?: string | null };
export type MilestoneDto  = { id: string; title: string; amount: number; status: string; dueDate: string };
export type ExpenseDto    = { id: string; title: string; amount: number; receiptUrl?: string | null; createdAt: string };

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './project-detail.component.html'
})
export class ProjectDetailComponent {
  private readonly http  = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly fb    = inject(FormBuilder);
  readonly auth          = inject(AuthService);

  projectId: string | null = null;
  project:   ProjectDto | null = null;
  milestones: MilestoneDto[] = [];
  expenses:   ExpenseDto[]   = [];
  balance = 0;
  error: string | null = null;

  activeTab: 'milestones' | 'expenses' | 'photos' = 'milestones';

  funding         = false;
  addingMilestone = false;
  addingExpense   = false;

  showMilestoneForm = false;
  showExpenseForm   = false;

  fundForm = this.fb.group({ amount: [1000, [Validators.required, Validators.min(1)]] });

  milestoneForm = this.fb.group({
    title:   ['', [Validators.required]],
    amount:  [0,  [Validators.required, Validators.min(1)]],
    dueDate: ['', [Validators.required]]
  });

  expenseForm = this.fb.group({
    title:      ['', [Validators.required]],
    amount:     [0,  [Validators.required, Validators.min(1)]],
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
      next:  (p) => (this.project = p),
      error: ()  => (this.error = 'Failed to load project')
    });
    this.http.get<MilestoneDto[]>(`/api/projects/${this.projectId}/milestones`).subscribe({
      next:  (m) => (this.milestones = m),
      error: ()  => (this.error = 'Failed to load milestones')
    });
    this.http.get<ExpenseDto[]>(`/api/projects/${this.projectId}/expenses`).subscribe({
      next:  (e) => (this.expenses = e),
      error: ()  => (this.error = 'Failed to load expenses')
    });
    this.http.get<{ balance: number }>(`/api/payments/projects/${this.projectId}/balance`).subscribe({
      next:  (b) => (this.balance = b.balance),
      error: ()  => {}
    });
  }

  fundEscrow() {
    if (!this.projectId || this.fundForm.invalid) return;
    this.funding = true;
    const amount = Number(this.fundForm.getRawValue().amount);
    this.http.post(`/api/payments/projects/${this.projectId}/fund`, { amount }).subscribe({
      next:  () => { this.funding = false; this.refresh(); },
      error: (e) => { this.funding = false; this.error = e?.error?.message ?? 'Funding failed'; }
    });
  }

  addMilestone() {
    if (!this.projectId || this.milestoneForm.invalid) return;
    this.addingMilestone = true;
    const v = this.milestoneForm.getRawValue();
    this.http.post(`/api/projects/${this.projectId}/milestones`, {
      title: v.title, amount: v.amount, dueDate: new Date(v.dueDate!).toISOString()
    }).subscribe({
      next: () => {
        this.addingMilestone = false;
        this.showMilestoneForm = false;
        this.milestoneForm.reset({ title: '', amount: 0, dueDate: '' });
        this.refresh();
      },
      error: (e) => { this.addingMilestone = false; this.error = e?.error?.message ?? 'Add milestone failed'; }
    });
  }

  addExpense() {
    if (!this.projectId || this.expenseForm.invalid) return;
    this.addingExpense = true;
    const v = this.expenseForm.getRawValue();
    this.http.post(`/api/projects/${this.projectId}/expenses`, v).subscribe({
      next: () => {
        this.addingExpense = false;
        this.showExpenseForm = false;
        this.expenseForm.reset({ title: '', amount: 0, receiptUrl: '' });
        this.refresh();
      },
      error: (e) => { this.addingExpense = false; this.error = e?.error?.message ?? 'Add expense failed'; }
    });
  }

  requestRelease(milestoneId: string) {
    this.http.post(`/api/payments/milestones/${milestoneId}/request-release`, {}).subscribe({
      next:  () => this.refresh(),
      error: (e) => (this.error = e?.error?.message ?? 'Request failed')
    });
  }

  approveRelease(milestoneId: string) {
    this.http.post(`/api/payments/milestones/${milestoneId}/approve-release`, {}).subscribe({
      next:  () => this.refresh(),
      error: (e) => (this.error = e?.error?.message ?? 'Approval failed')
    });
  }

  get totalExpenses(): number {
    return this.expenses.reduce((s, e) => s + e.amount, 0);
  }

  get completedMilestones(): number {
    return this.milestones.filter(m => m.status === 'Completed' || m.status === 'Released').length;
  }

  get progressPercent(): number {
    if (!this.milestones.length) return 0;
    return Math.round((this.completedMilestones / this.milestones.length) * 100);
  }

  milestoneStatusClass(status: string): string {
    const map: Record<string, string> = {
      Pending:          'bp-badge-gray',
      InProgress:       'bp-badge-blue',
      RequestedRelease: 'bp-badge-yellow',
      Completed:        'bp-badge-green',
      Released:         'bp-badge-green'
    };
    return map[status] ?? 'bp-badge-gray';
  }
}

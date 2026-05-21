import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { ProjectDto, MilestoneDto, ExpenseDto, PhotoDto } from './project.models';

@Component({
  selector: 'app-customer-project-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './project-detail.component.html'
})
export class CustomerProjectDetailComponent {
  private readonly http   = inject(HttpClient);
  private readonly route  = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb     = inject(FormBuilder);
  readonly auth           = inject(AuthService);

  readonly roles = this.auth.roles$;

  projectId = signal<string | null>(null);
  project   = signal<ProjectDto | null>(null);
  milestones = signal<MilestoneDto[]>([]);
  expenses   = signal<ExpenseDto[]>([]);
  photos     = signal<PhotoDto[]>([]);
  balance    = signal(0);
  error      = signal<string | null>(null);
  loading    = signal(true);

  activeTab = signal<'milestones' | 'expenses' | 'photos'>('milestones');

  // ── Edit project ──────────────────────────────────────────────────────
  showEditForm = signal(false);
  saving       = signal(false);
  editForm = this.fb.group({
    name:   ['', [Validators.required, Validators.minLength(3)]],
    budget: [0,  [Validators.required, Validators.min(1)]]
  });

  // ── Assign contractor ─────────────────────────────────────────────────
  showAssignForm  = signal(false);
  assigning       = signal(false);
  assignError     = signal<string | null>(null);
  assignForm = this.fb.group({
    contractorUserId: ['', [Validators.required,
      Validators.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)]]
  });

  // ── Milestones ────────────────────────────────────────────────────────
  showMilestoneForm = signal(false);
  addingMilestone   = signal(false);
  milestoneError    = signal<string | null>(null);
  deletingMilestoneId = signal<string | null>(null);
  milestoneForm = this.fb.group({
    title:   ['', [Validators.required, Validators.maxLength(200)]],
    amount:  [0,  [Validators.required, Validators.min(1)]],
    dueDate: ['', [Validators.required]]
  });

  // ── Expenses ──────────────────────────────────────────────────────────
  showExpenseForm = signal(false);
  addingExpense   = signal(false);
  expenseError    = signal<string | null>(null);
  deletingExpenseId = signal<string | null>(null);
  expenseForm = this.fb.group({
    title:      ['', [Validators.required, Validators.maxLength(200)]],
    amount:     [0,  [Validators.required, Validators.min(1)]],
    receiptUrl: ['']
  });

  // ── Photos ────────────────────────────────────────────────────────────
  showPhotoForm = signal(false);
  addingPhoto   = signal(false);
  photoError    = signal<string | null>(null);
  photoForm = this.fb.group({
    url:     ['', [Validators.required]],
    caption: ['']
  });

  // ── Escrow ────────────────────────────────────────────────────────────
  funding  = signal(false);
  fundForm = this.fb.group({ amount: [1000, [Validators.required, Validators.min(1)]] });

  // ── Computed stats ────────────────────────────────────────────────────
  readonly totalExpenses = computed(() =>
    this.expenses().reduce((s, e) => s + e.amount, 0));

  readonly completedMilestones = computed(() =>
    this.milestones().filter(m => m.status === 'Completed' || m.status === 'Approved').length);

  readonly progressPercent = computed(() => {
    const total = this.milestones().length;
    return total === 0 ? 0 : Math.round((this.completedMilestones() / total) * 100);
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    this.projectId.set(id);
    if (id) this.refresh();
  }

  refresh() {
    const id = this.projectId();
    if (!id) return;
    this.loading.set(true);
    this.error.set(null);

    this.http.get<ProjectDto>(`/api/projects/${id}`).subscribe({
      next: (p) => {
        this.project.set(p);
        this.editForm.patchValue({ name: p.name, budget: p.budget });
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.error.set('Failed to load project'); }
    });

    this.http.get<MilestoneDto[]>(`/api/projects/${id}/milestones`).subscribe({
      next: (m) => this.milestones.set(m), error: () => {}
    });
    this.http.get<ExpenseDto[]>(`/api/projects/${id}/expenses`).subscribe({
      next: (e) => this.expenses.set(e), error: () => {}
    });
    this.http.get<PhotoDto[]>(`/api/projects/${id}/photos`).subscribe({
      next: (ph) => this.photos.set(ph), error: () => {}
    });
    this.http.get<{ balance: number }>(`/api/payments/projects/${id}/balance`).subscribe({
      next: (b) => this.balance.set(b.balance), error: () => {}
    });
  }

  // ── Edit project ──────────────────────────────────────────────────────
  saveEdit() {
    if (this.editForm.invalid) return;
    const id = this.projectId()!;
    const { name, budget } = this.editForm.getRawValue();
    this.saving.set(true);
    this.http.put(`/api/projects/${id}`, { name, budget }).subscribe({
      next: () => { this.saving.set(false); this.showEditForm.set(false); this.refresh(); },
      error: (e) => { this.saving.set(false); this.error.set(e?.error?.message ?? 'Update failed'); }
    });
  }

  updateStatus(status: string) {
    const id = this.projectId()!;
    this.http.put(`/api/projects/${id}`, { status }).subscribe({
      next: () => this.refresh(),
      error: (e) => this.error.set(e?.error?.message ?? 'Status update failed')
    });
  }

  // ── Assign contractor ─────────────────────────────────────────────────
  assignContractor() {
    if (this.assignForm.invalid) return;
    const id = this.projectId()!;
    const { contractorUserId } = this.assignForm.getRawValue();
    this.assigning.set(true);
    this.assignError.set(null);
    this.http.post(`/api/projects/${id}/assign`, { contractorUserId }).subscribe({
      next: () => { this.assigning.set(false); this.showAssignForm.set(false); this.assignForm.reset(); this.refresh(); },
      error: (e) => { this.assigning.set(false); this.assignError.set(e?.error?.message ?? 'Assignment failed'); }
    });
  }

  // ── Milestones ────────────────────────────────────────────────────────
  addMilestone() {
    if (this.milestoneForm.invalid) return;
    const id = this.projectId()!;
    const v = this.milestoneForm.getRawValue();
    this.addingMilestone.set(true);
    this.milestoneError.set(null);
    this.http.post(`/api/projects/${id}/milestones`, {
      title: v.title, amount: v.amount,
      dueDateUtc: new Date(v.dueDate!).toISOString()
    }).subscribe({
      next: () => {
        this.addingMilestone.set(false);
        this.showMilestoneForm.set(false);
        this.milestoneForm.reset({ title: '', amount: 0, dueDate: '' });
        this.refresh();
      },
      error: (e) => { this.addingMilestone.set(false); this.milestoneError.set(e?.error?.message ?? 'Failed to add milestone'); }
    });
  }

  updateMilestoneStatus(milestoneId: string, status: string) {
    const id = this.projectId()!;
    this.http.put(`/api/projects/${id}/milestones/${milestoneId}/status`, { status }).subscribe({
      next: () => this.refresh(),
      error: (e) => this.error.set(e?.error?.message ?? 'Status update failed')
    });
  }

  deleteMilestone(milestoneId: string) {
    const id = this.projectId()!;
    this.http.delete(`/api/projects/${id}/milestones/${milestoneId}`).subscribe({
      next: () => { this.deletingMilestoneId.set(null); this.refresh(); },
      error: (e) => { this.deletingMilestoneId.set(null); this.error.set(e?.error?.message ?? 'Delete failed'); }
    });
  }

  // ── Expenses ──────────────────────────────────────────────────────────
  addExpense() {
    if (this.expenseForm.invalid) return;
    const id = this.projectId()!;
    const v = this.expenseForm.getRawValue();
    this.addingExpense.set(true);
    this.expenseError.set(null);
    this.http.post(`/api/projects/${id}/expenses`, v).subscribe({
      next: () => {
        this.addingExpense.set(false);
        this.showExpenseForm.set(false);
        this.expenseForm.reset({ title: '', amount: 0, receiptUrl: '' });
        this.refresh();
      },
      error: (e) => { this.addingExpense.set(false); this.expenseError.set(e?.error?.message ?? 'Failed to add expense'); }
    });
  }

  deleteExpense(expenseId: string) {
    const id = this.projectId()!;
    this.http.delete(`/api/projects/${id}/expenses/${expenseId}`).subscribe({
      next: () => { this.deletingExpenseId.set(null); this.refresh(); },
      error: (e) => { this.deletingExpenseId.set(null); this.error.set(e?.error?.message ?? 'Delete failed'); }
    });
  }

  // ── Photos ────────────────────────────────────────────────────────────
  addPhoto() {
    if (this.photoForm.invalid) return;
    const id = this.projectId()!;
    const v = this.photoForm.getRawValue();
    this.addingPhoto.set(true);
    this.photoError.set(null);
    this.http.post(`/api/projects/${id}/photos`, { url: v.url, caption: v.caption || null }).subscribe({
      next: () => {
        this.addingPhoto.set(false);
        this.showPhotoForm.set(false);
        this.photoForm.reset();
        this.refresh();
      },
      error: (e) => { this.addingPhoto.set(false); this.photoError.set(e?.error?.message ?? 'Failed to add photo'); }
    });
  }

  // ── Escrow ────────────────────────────────────────────────────────────
  fundEscrow() {
    if (this.fundForm.invalid) return;
    const id = this.projectId()!;
    const amount = this.fundForm.getRawValue().amount;
    this.funding.set(true);
    this.http.post(`/api/payments/projects/${id}/fund`, { amount }).subscribe({
      next: () => { this.funding.set(false); this.refresh(); },
      error: (e) => { this.funding.set(false); this.error.set(e?.error?.message ?? 'Funding failed'); }
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────
  milestoneStatusBadge(status: string): string {
    const map: Record<string, string> = {
      Pending: 'bp-badge-gray', Completed: 'bp-badge-blue', Approved: 'bp-badge-green'
    };
    return map[status] ?? 'bp-badge-gray';
  }

  isCustomer()   { return this.roles().includes('Customer'); }
  isContractor() { return this.roles().includes('Contractor'); }
  isAdmin()      { return this.roles().includes('Admin'); }
  canEdit()      { return this.isCustomer() || this.isAdmin(); }

  goBack() { this.router.navigate(['/features/customer/projects']); }
}

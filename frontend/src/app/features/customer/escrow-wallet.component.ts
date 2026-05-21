import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface ProjectBalance { projectId: string; name: string; balance: number; }
interface Transaction { id: string; projectId: string; milestoneId?: string; amount: number; direction: string; createdAtUtc: string; }

@Component({
  selector: 'app-escrow-wallet',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './escrow-wallet.component.html'
})
export class EscrowWalletComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  projects   = signal<any[]>([]);
  balances   = signal<ProjectBalance[]>([]);
  loading    = signal(true);
  error      = signal<string | null>(null);
  funding    = signal(false);
  fundError  = signal<string | null>(null);
  fundSuccess = signal<string | null>(null);

  fundForm = this.fb.group({
    projectId: ['', Validators.required],
    amount:    [0,  [Validators.required, Validators.min(1)]]
  });

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.http.get<any[]>('/api/projects/my').subscribe({
      next: (projects) => {
        this.projects.set(projects);
        // Fetch balance for each project
        const active = projects.filter(p => p.status === 'Active' || p.status === 'Draft');
        if (active.length === 0) { this.loading.set(false); return; }
        let done = 0;
        const balances: ProjectBalance[] = [];
        active.forEach(p => {
          this.http.get<{ balance: number }>(`/api/payments/projects/${p.id}/balance`).subscribe({
            next: (b) => {
              balances.push({ projectId: p.id, name: p.name, balance: b.balance });
              if (++done === active.length) { this.balances.set(balances); this.loading.set(false); }
            },
            error: () => { if (++done === active.length) { this.balances.set(balances); this.loading.set(false); } }
          });
        });
      },
      error: () => { this.loading.set(false); this.error.set('Failed to load projects'); }
    });
  }

  get totalEscrow(): number { return this.balances().reduce((s, b) => s + b.balance, 0); }

  fund() {
    if (this.fundForm.invalid) return;
    const { projectId, amount } = this.fundForm.getRawValue();
    this.funding.set(true);
    this.fundError.set(null);
    this.fundSuccess.set(null);
    this.http.post('/api/payments/escrow/fund', { projectId, amount }).subscribe({
      next: () => {
        this.funding.set(false);
        this.fundSuccess.set(`KES ${amount?.toLocaleString()} funded successfully.`);
        this.fundForm.patchValue({ amount: 0 });
        this.load();
      },
      error: (e) => { this.funding.set(false); this.fundError.set(e?.error?.message ?? 'Funding failed'); }
    });
  }
}

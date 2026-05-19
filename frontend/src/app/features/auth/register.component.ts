import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="mx-auto max-w-md rounded-2xl border bg-white p-6">
      <h2 class="text-xl font-semibold">Register</h2>
      <p class="mt-1 text-sm text-slate-600">Creates a Keycloak user and assigns a realm role.</p>

      <form class="mt-4 grid gap-3" [formGroup]="form" (ngSubmit)="submit()">
        <label class="grid gap-1 text-sm">
          <span>Full name</span>
          <input class="rounded-md border px-3 py-2" formControlName="fullName" />
        </label>
        <label class="grid gap-1 text-sm">
          <span>Email</span>
          <input class="rounded-md border px-3 py-2" formControlName="email" type="email" />
        </label>
        <label class="grid gap-1 text-sm">
          <span>Password</span>
          <input class="rounded-md border px-3 py-2" formControlName="password" type="password" />
        </label>
        <label class="grid gap-1 text-sm">
          <span>Role</span>
          <select class="rounded-md border px-3 py-2" formControlName="role">
            <option value="Customer">Customer</option>
            <option value="Contractor">Contractor</option>
            <option value="Supplier">Supplier</option>
            <option value="Driver">Driver</option>
          </select>
        </label>

        <button
          class="mt-2 rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60"
          [disabled]="form.invalid || loading"
          type="submit"
        >
          {{ loading ? 'Creating…' : 'Create account' }}
        </button>

        <div class="text-sm text-slate-600">
          Already have an account? <a routerLink="/login" class="underline">Login</a>
        </div>

        <div *ngIf="success" class="rounded-md border border-emerald-200 bg-emerald-50 p-2 text-sm text-emerald-800">
          Account created. You can now login.
        </div>
        <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
      </form>
    </div>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  success = false;
  error: string | null = null;

  form = this.fb.group({
    fullName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Customer', [Validators.required]]
  });

  submit() {
    if (this.form.invalid) return;
    const { fullName, email, password, role } = this.form.getRawValue();

    this.loading = true;
    this.success = false;
    this.error = null;

    this.auth.register(fullName!, email!, password!, role!).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
        setTimeout(() => this.router.navigateByUrl('/login'), 600);
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Registration failed';
      }
    });
  }
}


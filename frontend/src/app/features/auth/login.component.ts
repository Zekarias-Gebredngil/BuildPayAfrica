import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="mx-auto max-w-md rounded-2xl border bg-white p-6">
      <h2 class="text-xl font-semibold">Login</h2>
      <p class="mt-1 text-sm text-slate-600">Use your email and password (Keycloak direct grant).</p>

      <form class="mt-4 grid gap-3" [formGroup]="form" (ngSubmit)="submit()">
        <label class="grid gap-1 text-sm">
          <span>Email</span>
          <input class="rounded-md border px-3 py-2" formControlName="email" type="email" />
        </label>
        <label class="grid gap-1 text-sm">
          <span>Password</span>
          <input class="rounded-md border px-3 py-2" formControlName="password" type="password" />
        </label>

        <button
          class="mt-2 rounded-md bg-slate-900 px-4 py-2 text-sm text-white disabled:opacity-60"
          [disabled]="form.invalid || loading"
          type="submit"
        >
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
        <div class="text-sm text-slate-600">
          No account? <a routerLink="/register" class="underline">Register</a>
        </div>
        <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
      </form>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  error: string | null = null;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  submit() {
    if (this.form.invalid) return;
    const { email, password } = this.form.getRawValue();
    this.loading = true;
    this.error = null;

    this.auth.login(email!, password!).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl('/');
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Login failed';
      }
    });
  }
}


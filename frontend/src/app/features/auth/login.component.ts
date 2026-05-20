import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route  = inject(ActivatedRoute);

  loading = false;
  error: string | null = null;
  showPassword = signal(false);

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  togglePassword() { this.showPassword.update(v => !v); }

  submit() {
    if (this.form.invalid) return;
    const { email, password } = this.form.getRawValue();
    this.loading = true;
    this.error = null;

    this.auth.login(email!, password!).subscribe({
      next: () => {
        this.loading = false;
        // Redirect to the originally requested URL, or home
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Invalid email or password. Please try again.';
      }
    });
  }
}

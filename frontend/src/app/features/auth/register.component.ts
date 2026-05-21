import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  success = false;
  error: string | null = null;
  showPassword = signal(false);

  readonly roles = [
    { value: 'Customer',   label: 'Customer',   icon: '🏗️', desc: 'Manage projects & hire contractors' },
    { value: 'Contractor', label: 'Contractor',  icon: '👷', desc: 'Find projects & get paid via escrow' },
    { value: 'Supplier',   label: 'Supplier',    icon: '🏭', desc: 'List products & fulfill orders' },
    { value: 'Driver',     label: 'Driver',      icon: '🚚', desc: 'Deliver materials & track routes' }
  ];

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role:     ['Customer', [Validators.required]]
  });

  togglePassword() { this.showPassword.update(v => !v); }

  selectRole(role: string) {
    this.form.patchValue({ role });
  }

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
        setTimeout(() => this.router.navigateByUrl('/login'), 1500);
      },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? 'Registration failed. Please try again.';
      }
    });
  }
}

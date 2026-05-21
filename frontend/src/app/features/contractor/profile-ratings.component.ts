import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

interface Profile { userId: string; displayName: string; bio?: string; ratingAverage: number; ratingCount: number; }

@Component({
  selector: 'app-profile-ratings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-ratings.component.html'
})
export class ProfileRatingsComponent {
  private readonly http = inject(HttpClient);
  private readonly fb   = inject(FormBuilder);

  profile  = signal<Profile | null>(null);
  loading  = signal(true);
  saving   = signal(false);
  error    = signal<string | null>(null);
  success  = signal(false);

  form = this.fb.group({
    displayName: ['', [Validators.required, Validators.maxLength(200)]],
    bio:         ['', [Validators.maxLength(2000)]]
  });

  constructor() {
    this.http.get<Profile>('/api/profiles/me').subscribe({
      next: (p) => {
        this.profile.set(p);
        this.form.patchValue({ displayName: p.displayName, bio: p.bio ?? '' });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save() {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set(null);
    const { displayName, bio } = this.form.getRawValue();
    this.http.put('/api/profiles/me', { displayName, bio }).subscribe({
      next:  () => { this.saving.set(false); this.success.set(true); setTimeout(() => this.success.set(false), 3000); },
      error: (e) => { this.saving.set(false); this.error.set(e?.error?.message ?? 'Save failed'); }
    });
  }

  stars(avg: number) { return '★'.repeat(Math.round(avg)) + '☆'.repeat(5 - Math.round(avg)); }
}

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

interface ContractorProfile {
  userId: string; displayName: string; bio?: string;
  ratingAverage: number; ratingCount: number;
}

@Component({
  selector: 'app-contractor-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contractor-search.component.html'
})
export class ContractorSearchComponent {
  private readonly http = inject(HttpClient);

  contractors = signal<ContractorProfile[]>([]);
  loading     = signal(true);
  error       = signal<string | null>(null);
  search      = signal('');

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    // UserProfile service — list verified contractors
    this.http.get<ContractorProfile[]>('/api/profiles?role=Contractor').subscribe({
      next:  (res) => { this.contractors.set(res); this.loading.set(false); },
      error: ()    => { this.loading.set(false); this.error.set('Failed to load contractors'); }
    });
  }

  get filtered() {
    const q = this.search().toLowerCase();
    return q ? this.contractors().filter(c => c.displayName.toLowerCase().includes(q)) : this.contractors();
  }

  stars(avg: number): string { return '★'.repeat(Math.round(avg)) + '☆'.repeat(5 - Math.round(avg)); }
}

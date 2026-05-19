import { Component } from '@angular/core';

@Component({
  selector: 'app-about',
  standalone: true,
  template: `
    <div class="rounded-2xl border bg-white p-6">
      <h2 class="text-xl font-semibold">About BuildPay Africa</h2>
      <p class="mt-2 text-slate-600">
        This MVP implements procurement, project milestones, escrow releases, deliveries, contractor verification, and an admin overview.
      </p>
    </div>
  `
})
export class AboutComponent {}


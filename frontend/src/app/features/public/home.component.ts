import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <h1 class="text-2xl font-semibold tracking-tight">Smart construction procurement and milestone payments.</h1>
        <p class="mt-2 text-slate-600">
          BuildPay Africa helps customers, contractors, suppliers, and drivers manage materials, projects, deliveries, and escrow releases.
        </p>
        <div class="mt-4 flex flex-wrap gap-2">
          <a routerLink="/register" class="rounded-md bg-slate-900 px-4 py-2 text-sm text-white">Create account</a>
          <a routerLink="/marketplace" class="rounded-md border px-4 py-2 text-sm hover:bg-slate-100">Browse marketplace</a>
        </div>
      </div>

      <div class="grid gap-4 md:grid-cols-3">
        <div class="rounded-xl border bg-white p-4">
          <div class="text-sm font-semibold">Marketplace</div>
          <div class="mt-1 text-sm text-slate-600">List products, search, order materials.</div>
        </div>
        <div class="rounded-xl border bg-white p-4">
          <div class="text-sm font-semibold">Projects</div>
          <div class="mt-1 text-sm text-slate-600">Create projects, milestones, expenses, photo updates.</div>
        </div>
        <div class="rounded-xl border bg-white p-4">
          <div class="text-sm font-semibold">Escrow</div>
          <div class="mt-1 text-sm text-slate-600">Fund escrow and approve milestone payments.</div>
        </div>
      </div>
    </section>
  `
})
export class HomeComponent {}


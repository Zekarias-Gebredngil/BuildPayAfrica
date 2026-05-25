import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  template: `
    <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:1rem;margin-bottom:1.5rem;flex-wrap:wrap;">
      <div class="bp-page-header" style="margin:0;"><h2>👥 User Management</h2><p>Manage platform users, roles, and access.</p></div>
    </div>
    <div class="bp-card bp-fade-in">
      <div class="bp-empty-state">
        <div class="bp-empty-icon">👥</div>
        <h3>Coming Soon</h3>
        <p>Full user management features will be available in the next release.</p>
      </div>
    </div>
  `
})
export class AdminUsersComponent {}

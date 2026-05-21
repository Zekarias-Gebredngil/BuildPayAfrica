import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guard';
import { roleGuard } from '../../core/auth/role.guard';
import { ContractorDashboardComponent } from './dashboard.component';
import { ContractorVerificationComponent } from './contractor-verification.component';
import { MilestoneUpdatesComponent } from './milestone-updates.component';
import { PaymentRequestsComponent } from './payment-requests.component';
import { EarningsHistoryComponent } from './earnings-history.component';
import { ProfileRatingsComponent } from './profile-ratings.component';
import { ProjectsComponent } from '../projects/projects.component';
import { NotificationsComponent } from '../notifications/notifications.component';

export const CONTRACTOR_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Contractor', 'Admin'] },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: ContractorDashboardComponent },
      { path: 'verification', component: ContractorVerificationComponent },
      { path: 'projects', component: ProjectsComponent },
      { path: 'milestones', component: MilestoneUpdatesComponent },
      { path: 'payments', component: PaymentRequestsComponent },
      { path: 'earnings', component: EarningsHistoryComponent },
      { path: 'profile', component: ProfileRatingsComponent },
      { path: 'notifications', component: NotificationsComponent }
    ]
  }
];

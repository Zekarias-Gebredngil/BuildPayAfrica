import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';
import { authGuard } from '../../core/auth/auth.guard';
import { CustomerDashboardComponent } from './dashboard.component';
import { MilestoneTrackerComponent } from './milestone-tracker.component';
import { EscrowWalletComponent } from './escrow-wallet.component';
import { ContractorSearchComponent } from './contractor-search.component';
import { DeliveryTrackingComponent } from './delivery-tracking.component';
import { ProjectsComponent } from '../projects/projects.component';
import { OrdersComponent } from '../orders/orders.component';
import { NotificationsComponent } from '../notifications/notifications.component';
import { CartComponent } from '../marketplace/cart.component';
import { MarketplaceComponent } from '../marketplace/marketplace.component';

export const CUSTOMER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer', 'Admin'] },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: CustomerDashboardComponent },
      { path: 'projects', component: ProjectsComponent },
      { path: 'milestones', component: MilestoneTrackerComponent },
      { path: 'wallet', component: EscrowWalletComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'contractors', component: ContractorSearchComponent },
      { path: 'deliveries', component: DeliveryTrackingComponent },
      { path: 'notifications', component: NotificationsComponent },
      { path: 'marketplace', component: MarketplaceComponent },
      { path: 'cart', component: CartComponent }
    ]
  }
];

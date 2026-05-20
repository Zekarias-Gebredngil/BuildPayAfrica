import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './about.component.html'
})
export class AboutComponent {
  readonly features = [
    { icon: '🔒', title: 'Escrow Payments',       desc: 'Funds held securely until milestones are verified and approved.' },
    { icon: '📋', title: 'Project Management',     desc: 'Create projects, track milestones, log expenses, and upload photos.' },
    { icon: '🏪', title: 'Marketplace',            desc: 'Browse and order materials from verified suppliers across Africa.' },
    { icon: '👷', title: 'Contractor Network',     desc: 'Find and hire verified contractors with ratings and reviews.' },
    { icon: '🚚', title: 'Delivery Tracking',      desc: 'Real-time delivery status with driver info and ETA.' },
    { icon: '📊', title: 'Analytics & Reports',    desc: 'Comprehensive dashboards for suppliers, customers, and admins.' },
    { icon: '🔔', title: 'Smart Notifications',    desc: 'Stay updated on every project event and payment release.' },
    { icon: '🌍', title: 'Built for Africa',       desc: 'Designed for African construction workflows and payment systems.' }
  ];
}

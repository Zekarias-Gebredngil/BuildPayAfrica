import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-escrow',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './escrow.component.html'
})
export class AdminEscrowComponent {
}

import { Component, input } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-placeholder',
  template: `
    <h2>{{ title() }}</h2>
    <p class="bp-text-muted">Coming soon.</p>
  `
})
export class PlaceholderComponent {
  title = input<string>('Page');
}


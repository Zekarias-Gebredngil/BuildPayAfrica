import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';

import { AuthService } from '../core/auth/auth.service';
import { ShellComponent } from './shell.component';

class AuthServiceStub {
  readonly isAuthenticated$ = signal(true);
  readonly roles$ = signal<string[]>(['Customer']);
  readonly displayName$ = signal('Amina Builder');
  logout = jasmine.createSpy('logout');
}

describe('ShellComponent', () => {
  let fixture: ComponentFixture<ShellComponent>;
  let component: ShellComponent;
  let auth: AuthServiceStub;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useClass: AuthServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ShellComponent);
    component = fixture.componentInstance;
    auth = TestBed.inject(AuthService) as unknown as AuthServiceStub;
  });

  it('closes the mobile menu when the viewport becomes desktop sized', () => {
    component.onResize(640);
    component.toggleMobile();

    component.onResize(1024);

    expect(component.isMobile()).toBeFalse();
    expect(component.mobileOpen()).toBeFalse();
  });

  it('closes the mobile drawer after selecting an authenticated nav item', () => {
    component.onResize(640);
    component.toggleMobile();
    fixture.detectChanges();

    const firstNavLink = fixture.debugElement.query(
      By.css('[data-testid="authenticated-nav"] a')
    );
    firstNavLink.triggerEventHandler('click');

    expect(component.mobileOpen()).toBeFalse();
  });

  it('keeps the desktop collapse toggle from opening the mobile menu', () => {
    component.onResize(1024);

    component.toggleSidebar();

    expect(component.sidebarCollapsed()).toBeTrue();
    expect(component.mobileOpen()).toBeFalse();
  });

  it('delegates logout and closes the mobile drawer', () => {
    component.onResize(640);
    component.toggleMobile();

    component.logout();

    expect(auth.logout).toHaveBeenCalled();
    expect(component.mobileOpen()).toBeFalse();
  });

  it('renders feature-specific material icons in the header and sidebar', () => {
    fixture.detectChanges();

    const iconNames = fixture.debugElement
      .queryAll(By.css('mat-icon'))
      .map((icon) => icon.nativeElement.textContent.trim());

    expect(iconNames).toContain('notifications');
    expect(iconNames).toContain('dashboard');
    expect(iconNames).toContain('folder_open');
    expect(iconNames).toContain('shopping_cart');
  });

  it('exposes the current year for the footer copyright', () => {
    expect(component.currentYear).toBe(new Date().getFullYear());
  });
});

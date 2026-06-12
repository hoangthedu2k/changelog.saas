import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ProjectService } from '../../../core/services/project.service';
import { BillingService } from '../../../core/services/billing.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Project } from '../../../core/models/project.model';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  projectService = inject(ProjectService);
  billingService = inject(BillingService);
  authService = inject(AuthService);

  userInitials = computed(() => {
    const user = this.authService.currentUser();
    if (!user) return '?';
    const name = user.displayName || user.email;
    return name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
  });

  dropdownOpen = signal(false);

  isFree = computed(() => {
    const sub = this.billingService.subscription();
    return sub === null || sub.plan === 'Free';
  });

  mainNavItems: NavItem[] = [
    { label: 'Dashboard', path: '/app/dashboard', icon: '▦' },
    { label: 'Entries', path: '/app/entries', icon: '✎' },
    { label: 'Subscribers', path: '/app/subscribers', icon: '◎' },
    { label: 'Widget', path: '/app/widget', icon: '◧' },
  ];

  settingsNavItems: NavItem[] = [
    { label: 'Appearance', path: '/app/settings/appearance', icon: '◈' },
    { label: 'Project', path: '/app/settings/project', icon: '⊞' },
    { label: 'Custom Domain', path: '/app/settings/custom-domain', icon: '◉' },
  ];

  toggleDropdown() {
    this.dropdownOpen.update(v => !v);
  }

  selectProject(project: Project) {
    this.projectService.setActive(project);
    this.dropdownOpen.set(false);
  }
}

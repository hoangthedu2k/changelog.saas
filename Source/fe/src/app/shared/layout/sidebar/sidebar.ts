import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ProjectService } from '../../../core/services/project.service';
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

  dropdownOpen = signal(false);

  mainNavItems: NavItem[] = [
    { label: 'Dashboard', path: '/app/dashboard', icon: '▦' },
    { label: 'Entries', path: '/app/entries', icon: '✎' },
    { label: 'Subscribers', path: '/app/subscribers', icon: '◎' },
    { label: 'Widget', path: '/app/widget', icon: '◧' },
  ];

  settingsNavItems: NavItem[] = [
    { label: 'Appearance', path: '/app/settings/appearance', icon: '◈' },
    { label: 'Billing', path: '/app/settings/billing', icon: '▣' },
    { label: 'Project', path: '/app/settings/project', icon: '⊞' },
  ];

  toggleDropdown() {
    this.dropdownOpen.update(v => !v);
  }

  selectProject(project: Project) {
    this.projectService.setActive(project);
    this.dropdownOpen.set(false);
  }
}

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  navItems: NavItem[] = [
    { label: 'Dashboard', path: '/app/dashboard', icon: '▦' },
    { label: 'Entries', path: '/app/entries', icon: '✎' },
    { label: 'Subscribers', path: '/app/subscribers', icon: '◎' },
    { label: 'Settings', path: '/app/settings', icon: '⚙' },
  ];
}

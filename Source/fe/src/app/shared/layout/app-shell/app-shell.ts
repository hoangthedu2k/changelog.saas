import { afterNextRender, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Sidebar } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { Toast } from '../../components/toast/toast';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule, Sidebar, Topbar, Toast],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  private projectService = inject(ProjectService);

  constructor() {
    afterNextRender(() => {
      this.projectService.loadProjects().subscribe();
    });
  }
}

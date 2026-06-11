import { afterNextRender, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { Toast } from '../../components/toast/toast';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-app-shell',
  standalone: true,
  imports: [RouterOutlet, Sidebar, Topbar, Toast],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  private projectService = inject(ProjectService);

  constructor() {
    // afterNextRender only executes in the browser — safe to make HTTP calls here
    afterNextRender(() => {
      this.projectService.loadProjects().subscribe();
    });
  }
}

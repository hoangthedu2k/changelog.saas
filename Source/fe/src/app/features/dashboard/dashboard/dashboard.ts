import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardStats } from '../dashboard.service';
import { ProjectService } from '../../../core/services/project.service';
import { Entry } from '../../../core/models/entry.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  projectService = inject(ProjectService);
  private dashboardService = inject(DashboardService);

  stats = signal<DashboardStats>({ total: 0, published: 0, draft: 0 });
  recentEntries = signal<Entry[]>([]);
  loading = signal(false);

  constructor() {
    effect(() => {
      const project = this.projectService.activeProject();
      if (!project) return;

      this.loading.set(true);
      this.dashboardService.getRecentEntries(project.id).subscribe({
        next: entries => this.recentEntries.set(entries),
      });
      this.dashboardService.getStats(project.id).subscribe({
        next: s => { this.stats.set(s); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
    });
  }
}

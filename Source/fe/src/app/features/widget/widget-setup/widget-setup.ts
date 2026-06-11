import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-widget-setup',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './widget-setup.html',
  styleUrl: './widget-setup.scss',
})
export class WidgetSetup {
  projectService = inject(ProjectService);

  copied = signal(false);

  embedCode = computed(() => {
    const p = this.projectService.activeProject();
    if (!p) return '';
    const apiOrigin = environment.apiUrl.replace('/api', '');
    return `<script src="${apiOrigin}/widget.js" data-project="${p.slug}" data-public="${environment.publicUrl}"></script>`;
  });

  copyCode(): void {
    const code = this.embedCode();
    if (!code) return;
    navigator.clipboard.writeText(code).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    });
  }
}

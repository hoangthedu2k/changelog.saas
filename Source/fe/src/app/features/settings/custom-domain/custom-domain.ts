import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-custom-domain',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './custom-domain.html',
  styleUrl: './custom-domain.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomDomain {
  private projectService = inject(ProjectService);
  private toast = inject(ToastService);

  saving = signal(false);
  saved = signal(false);
  removing = signal(false);

  domainControl = new FormControl('', [
    Validators.pattern(/^([a-z0-9-]+\.)+[a-z]{2,}$/i),
  ]);

  activeProject = this.projectService.activeProject;

  constructor() {
    effect(() => {
      const p = this.projectService.activeProject();
      this.domainControl.setValue(p?.customDomain ?? '', { emitEvent: false });
      this.saved.set(false);
    });
  }

  save() {
    if (this.domainControl.invalid || this.saving()) return;
    const project = this.projectService.activeProject();
    if (!project) return;

    this.saving.set(true);
    this.projectService.update(project.id, {
      id: project.id,
      name: project.name,
      slug: project.slug,
      accentColor: project.accentColor,
      widgetPosition: project.widgetPosition,
      isPublic: project.isPublic,
      customDomain: this.domainControl.value?.trim() || undefined,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
        this.toast.success('Custom domain saved.');
        setTimeout(() => this.saved.set(false), 2000);
      },
      error: () => this.saving.set(false),
    });
  }

  remove() {
    const project = this.projectService.activeProject();
    if (!project || this.removing()) return;

    this.removing.set(true);
    this.projectService.update(project.id, {
      id: project.id,
      name: project.name,
      slug: project.slug,
      accentColor: project.accentColor,
      widgetPosition: project.widgetPosition,
      isPublic: project.isPublic,
      customDomain: undefined,
    }).subscribe({
      next: () => {
        this.removing.set(false);
        this.domainControl.setValue('', { emitEvent: false });
        this.toast.success('Custom domain removed.');
      },
      error: () => this.removing.set(false),
    });
  }
}

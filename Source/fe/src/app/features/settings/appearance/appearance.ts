import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { ToastService } from '../../../core/services/toast.service';
import { WidgetPosition } from '../../../core/models/project.model';
import { parseApiError } from '../../../core/http/parse-api-error';

const ACCENT_COLORS = ['#6366f1', '#0ea5e9', '#10b981', '#f59e0b', '#ef4444', '#ec4899', '#1a1714'];

const WIDGET_POSITIONS: { value: WidgetPosition; label: string }[] = [
  { value: 'tl', label: 'Top left' },
  { value: 'tr', label: 'Top right' },
  { value: 'bl', label: 'Bottom left' },
  { value: 'br', label: 'Bottom right' },
];

@Component({
  selector: 'app-appearance',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './appearance.html',
  styleUrl: './appearance.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Appearance {
  projectService = inject(ProjectService);
  private toast = inject(ToastService);

  accentColors = ACCENT_COLORS;
  widgetPositions = WIDGET_POSITIONS;
  saving = signal(false);
  saved = signal(false);

  form = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    accentColor: new FormControl('#6366f1', Validators.required),
    widgetPosition: new FormControl<WidgetPosition>('br', Validators.required),
  });

  constructor() {
    effect(() => {
      const p = this.projectService.activeProject();
      if (p) {
        this.form.patchValue({
          name: p.name,
          accentColor: p.accentColor,
          widgetPosition: p.widgetPosition,
        }, { emitEvent: false });
      }
    });
  }

  selectColor(color: string) {
    this.form.patchValue({ accentColor: color });
  }

  selectPosition(pos: WidgetPosition) {
    this.form.patchValue({ widgetPosition: pos });
  }

  slugify(name: string): string {
    return name
      .trim()
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-|-$/g, '');
  }

  save() {
    if (this.form.invalid || this.saving()) return;

    const { name, accentColor, widgetPosition } = this.form.getRawValue();
    const active = this.projectService.activeProject();
    this.saving.set(true);

    const obs$ = active
      ? this.projectService.update(active.id, {
          id: active.id,
          name: name!,
          slug: active.slug,
          accentColor: accentColor!,
          widgetPosition: widgetPosition!,
          isPublic: active.isPublic,
          customDomain: active.customDomain,
        })
      : this.projectService.create({
          name: name!,
          slug: this.slugify(name!),
          accentColor: accentColor!,
          widgetPosition: widgetPosition!,
          isPublic: true,
        });

    obs$.subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
        setTimeout(() => this.saved.set(false), 2000);
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(parseApiError(err));
      },
    });
  }
}

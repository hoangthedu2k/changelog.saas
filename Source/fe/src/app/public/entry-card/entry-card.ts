import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { WidgetEntry } from '../../core/models/widget.model';

const TAG_COLORS: Record<string, string> = {
  'new feature': 'tag--new',
  'bug fix': 'tag--fix',
  'improvement': 'tag--improvement',
  'security': 'tag--security',
  'performance': 'tag--performance',
};

@Component({
  selector: 'app-entry-card',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './entry-card.html',
  styleUrl: './entry-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EntryCard {
  entry = input.required<WidgetEntry>();

  tagClass(tag: string): string {
    return TAG_COLORS[tag.toLowerCase()] ?? 'tag--default';
  }
}

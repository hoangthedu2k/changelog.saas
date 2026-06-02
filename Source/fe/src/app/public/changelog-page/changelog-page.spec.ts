import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangelogPage } from './changelog-page';

describe('ChangelogPage', () => {
  let component: ChangelogPage;
  let fixture: ComponentFixture<ChangelogPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChangelogPage],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangelogPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

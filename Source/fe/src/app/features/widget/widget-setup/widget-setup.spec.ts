import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WidgetSetup } from './widget-setup';

describe('WidgetSetup', () => {
  let component: WidgetSetup;
  let fixture: ComponentFixture<WidgetSetup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WidgetSetup],
    }).compileComponents();

    fixture = TestBed.createComponent(WidgetSetup);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

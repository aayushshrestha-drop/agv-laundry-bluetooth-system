import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseStationComponent } from './basestation.component';

describe('BaseStationComponent', () => {
  let component: BaseStationComponent;
  let fixture: ComponentFixture<BaseStationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BaseStationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BaseStationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

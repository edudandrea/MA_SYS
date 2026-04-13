/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { DashAcadService } from './DashAcad.service';

describe('Service: DashAcad', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DashAcadService]
    });
  });

  it('should ...', inject([DashAcadService], (service: DashAcadService) => {
    expect(service).toBeTruthy();
  }));
});

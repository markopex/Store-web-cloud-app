import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService {

  public eventObservable = new BehaviorSubject<Object>(null);

  constructor() { }

  refresh(){
    this.eventObservable.next(new Object());
  }
}

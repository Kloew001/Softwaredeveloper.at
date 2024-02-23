import { Injectable } from '@angular/core';
import { BehaviorSubject, map, shareReplay } from 'rxjs';
import { BaseCacheResolverService, CacheResolverService } from './cacheResolver.service';

@Injectable()
export class BehaviourSubjectCacheResolverService extends BaseCacheResolverService<BehaviorSubject<any>> {
  constructor() {
    super();
  }

  override resolve(key: any, valueAction: any, reload: boolean | null = null, timeToLeave: number | null = null) {

    var cache$ = this.get(key);

    if (!cache$) {

      cache$ = new BehaviorSubject<any>(null);
      this.set(key, cache$, valueAction, timeToLeave);
      this.resolveInternal(key, cache$, valueAction);
    }
    else if (reload == true) {
      this.resolveInternal(key, cache$, valueAction);
    }

    return cache$;
  }

  resolveInternal(key: any, cache$: BehaviorSubject<any>, valueAction: any) {

    valueAction()
      .pipe(
        shareReplay(1),
      )
      .subscribe(data => {
        //console.log(key);
        //console.log(data);
        cache$.next(data);
      });

    return cache$;
  }

  refreshKey(key: string) {
    var cache$ = this.cache.get(key);
    if (cache$) {
      this.resolveInternal(key, cache$.value, cache$.valueAction);
    }
  }

  refreshAll() {
    this.cache.forEach((value, key) => {
      this.resolveInternal(key, value.value, value.valueAction);
    });
  }
}

using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ammy.VisualStudio.Service.Extensions
{
    public static class ObservableExtensions
    {
        public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(disposable);
        }

        public static IObservable<TResult> ExecuteOnlyLast<T, TResult>(this IObservable<T> async, Func<T, IObservable<TResult>> operation)
        {
            return Observable.Create<TResult>(sub => {
                bool isExecuting = false;
                int isValueWaiting = 0;
                var isDisposed = false;

                var d = async.Subscribe(async val => {
                    if (isExecuting) {
                        isValueWaiting = 1;
                        return;
                    }

                    isExecuting = true;

                    do {
                        try {
                            var res = await operation(val).FirstOrDefaultAsync();
                            if (res != null)
                                sub.OnNext(res);
                            else
                                sub.OnCompleted();
                        } catch (Exception e) {
                            sub.OnError(e);
                        }
                    } while (Interlocked.Exchange(ref isValueWaiting, 0) == 1 && !isDisposed);

                    isExecuting = false;
                }, sub.OnCompleted);

                return Disposable.Create(() => {
                    d.Dispose();
                    isDisposed = true;
                });
            });
        }

        public static IObservable<Unit> MergeUnit<T, TOther>(this IObservable<T> async, IObservable<TOther> other)
        {
            return async.Select(_ => Unit.Default)
                        .Merge(other.Select(_ => Unit.Default));
        }

        public static IDisposable SubscribeOnce<T>(this IObservable<T> instance, Action<T> onNext)
        {
            return instance.Take(1)
                           .Subscribe(onNext);
        }
    }
}
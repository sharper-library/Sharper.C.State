using System;
using Sharper.C.Data;
using static Sharper.C.Data.Unit;

namespace Sharper.C.Control
{
    public static class State
    {
        public static State<S, A> Mk<S, A>(Func<S, And<A, S>> run)
        =>  new State<S, A>(run);

        public static State<S, A> Defer<S, A>(Func<A> a)
        =>  new State<S, A>(s => And.Mk(a(), s));

        public static State<S, A> Pure<S, A>(A a)
        =>  new State<S, A>(s => And.Mk(a, s));

        public static State<S, S> Get<S>()
        =>  new State<S, S>(s => And.Mk(s, s));

        public static State<S, Unit> Modify<S>(Func<S, S> f)
        =>  new State<S, Unit>(s => And.Mk(UNIT, f(s)));

        public static State<S, Unit> Set<S>(S s)
        =>  new State<S, Unit>(_ => And.Mk(UNIT, s));
    }

    public struct State<S, A>
    {
        private readonly Func<S, And<A, S>> run;

        internal State(Func<S, And<A, S>> run)
        {   this.run = run;
        }

        public State<S, B> Map<B>(Func<A, B> f)
        {   var go = run;
            return new State<S, B>(s => go(s).MapFst(f));
        }

        public State<S, B> Map<B>(Func<A, State<S, B>> f)
        {   var go = run;
            return
                new State<S, B>
                  ( s =>
                    {   var x = go(s);
                        return f(x.Fst).run(x.Snd);
                    }
                  );
        }

        public State<S, B> Select<B>(Func<A, B> f)
        =>  Map(f);

        public State<S, C> SelectMany<B, C>(Func<A, State<S, B>> f, Func<A, B, C> g)
        {   var go = run;
            return
                new State<S, C>
                  ( s =>
                    {   var x = go(s);
                        var y = f(x.Fst).run(x.Snd);
                        return And.Mk(g(x.Fst, y.Fst), y.Snd);
                    }
                  );
        }
    }
}

define([], function() {

    "use strict";

    function Timer(callback, period) {
        this.callback = callback;
        this.period = period;
        this.handler = null;
    }

    Timer.prototype.start = function() {
        this.handler = setInterval(this.callback, this.period);
    };

    Timer.prototype.stop = function() {
        clearInterval(this.handler);
    };

    return Timer;
});

define(['underscore'], function (_) {
    
    "use strict";

    _.bindAll = function(obj) {
        var funcs = Array.prototype.slice.call(arguments, 1);
        if (funcs.length === 0) funcs = _.functions(obj);
        _.each(funcs, function(f) {
            if (f !== 'constructor' && f != 'initialize') {
                obj[f] = _.bind(obj[f], obj);
            }
        });
        return obj;
    };

    return _;
});

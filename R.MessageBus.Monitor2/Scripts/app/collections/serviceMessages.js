define(['backbone'], function(Backbone) {

    "use strict";

    var collection = Backbone.Collection.extend({
        url: "serviceMessages"
    });

    return collection;
});

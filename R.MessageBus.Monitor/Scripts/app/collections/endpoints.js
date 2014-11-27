define(['backbone'], function(Backbone) {

    "use strict";

    var collection = Backbone.Collection.extend({
        url: "endpoints"
    });

    return collection;
});

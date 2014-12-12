define(['backbone'], function(Backbone) {

    "use strict";

    var collection = Backbone.Collection.extend({
        url: "services"
    });

    return collection;
});

define(['backbone'], function(Backbone) {

    "use strict";

    var model = Backbone.Model.extend({
        idAttribute: 'Id',
        urlRoot: "error"
    });

    return model;
});

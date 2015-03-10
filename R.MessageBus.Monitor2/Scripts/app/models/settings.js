define(['backbone'], function(Backbone) {

    "use strict";

    var model = Backbone.Model.extend({

        url: "settings",

        initialize: function() {
            _.bindAll(this);
            if (!(this.attributes.Environments instanceof Backbone.Collection)) {
                this.attributes.Environments = new Backbone.Collection(this.attributes.Environments);
            }
        },

        parse: function(data) {
            if (data) {
                data.Environments = new Backbone.Collection(data.Environments);
            }
            return data;
        },

        toJSON: function() {
            var attributes = _.clone(this.attributes);
            $.each(attributes, function(key, value) {
                if (value !== null && value !== undefined && _(value.toJSON).isFunction()) {
                    attributes[key] = value.toJSON();
                }
            });
            return attributes;
        }
    });

    return model;
});

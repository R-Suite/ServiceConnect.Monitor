define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/navigation.html'
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".navigation",

        events: {
            "click li > a": "_setActive"
        },

        initialize: function() {
            _.bindAll(this);
            Backbone.Hubs.ErrorHub.on("errors", this._addErrorIcon);
        },

        render: function() {
            this.$el.html(template);

            return this;
        },

        _setActive: function(e) {
            this.$el.find(".active").removeClass("active");
            $(e.currentTarget).parent().addClass("active");
            if ($(e.currentTarget).parent().hasClass("errors")) {
                this.$el.find(".error-icon").hide();
            }
        },

        setActive: function(route) {
            this.$el.find(".navigationItems").removeClass("active");

            try {
                this.$el.find("." + route).addClass("active");

            } catch (e) {}
        },

        _addErrorIcon: function() {
            if (!this.$el.find(".active").hasClass("errors")) {
                var errorIcon = this.$el.find(".error-icon");
                errorIcon.fadeIn(100)
                    .delay(100)
                    .fadeOut(100)
                    .fadeIn(100)
                    .delay(100)
                    .fadeOut(100)
                    .fadeIn(100)
                    .delay(100)
                    .fadeOut(100)
                    .fadeIn(100)
                    .delay(100)
                    .fadeOut(100)
                    .fadeIn(100);
            }
        },

        onClose: function() {
            Backbone.Hubs.ErrorHub.off("errors", this._addErrorIcon);
        }
    });

    return view;
});

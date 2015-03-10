//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

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

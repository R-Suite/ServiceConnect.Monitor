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

define(['backbone', 'underscore'], function (Backbone, _) {

    "use strict";

    (function() {

        var proxiedSync = Backbone.sync;

        Backbone.sync = function(method, model, options) {
            options = options || {};

            if (!options.crossDomain) {
                options.crossDomain = true;
            }

            if (!options.xhrFields) {
                options.xhrFields = {
                    withCredentials: true
                };
            }

            return proxiedSync(method, model, options);
        };

        Backbone.View.prototype.close = function(that) {
            if (that === undefined || that === null) {
                that = this;
            }
            that.undelegateEvents();
            that.unbind();
            if (that.onClose) {
                that.onClose();
            }
            that.model = null;
            that.collection = null;
            if (that.$el) {
                that.$el.empty();
                that.$el = null;
            }
            that.view = null;
            if (that.activeViews) {
                that.closeViews();
            }
        };

        var renderView = function(view, that) {
            if (that === undefined || that === null) {
                that = this;
            }
            if (that.activeViews === undefined || that.activeViews === null) {
                that.activeViews = [];
            }
            that.activeViews.push(view);
            view.render();
        };

        var closeViews = function(that) {
            if (that === undefined || that === null) {
                that = this;
            }
            if (that.activeViews) {
                for (var i = 0; i < that.activeViews.length; i++) {
                    that.activeViews[i].close();
                }
            }
            that.activeViews = [];
        };

        _.extend(Backbone.Router.prototype, Backbone.Events, {
            route: function(route, name, callback) {
                if (!_.isRegExp(route)) route = this._routeToRegExp(route);
                if (!callback) callback = this[name];
                Backbone.history.route(route, _.bind(function(fragment) {
                    var args = this._extractParameters(route, fragment);
                    if (this.before && _.isFunction(this.before)) {
                        this.before(fragment);
                    }
                    if (callback) {
                        callback.apply(this, args);
                    }

                    this.trigger.apply(this, ['route:' + name].concat(args));
                    if (this.after && _.isFunction(this.after)) {
                        this.after(fragment);
                    }
                    Backbone.history.trigger('route', this, name, args);
                }, this));
                return this;
            }
        });

        Backbone.View.prototype.renderView = renderView;
        Backbone.Router.prototype.renderView = renderView;
        Backbone.View.prototype.closeViews = closeViews;
        Backbone.Router.prototype.closeViews = closeViews;

    })();

    return Backbone;
});

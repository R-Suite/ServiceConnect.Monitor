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

define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/exceptionModal.html'
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        events: {
            "click .closeModal": "_closeModal"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            var prittyJson = JSON.stringify(this.model.get("Exception"), undefined, 2);
            this.$el.find(".exception").html(this._jsonSyntaxHighlight(prittyJson));
            this.$el.find(".exceptionModal").modal('show');
            this.$el.find(".exceptionModal").on('hidden.bs.modal', this.closeView);
            return this;
        },

        _closeModal: function() {
            this.$el.find(".exceptionModal").modal('hide');
        },

        _jsonSyntaxHighlight: function(json) {
            if (typeof json !== 'string') {
                json = JSON.stringify(json, undefined, 2);
            }

            json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

            return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function(match) {
                var cls = 'number';
                if (/^"/.test(match)) {
                    if (/:$/.test(match)) {
                        cls = 'key';
                    } else {
                        cls = 'string';
                    }
                } else if (/true|false/.test(match)) {
                    cls = 'boolean';
                } else if (/null/.test(match)) {
                    cls = 'null';
                }
                return '<span class="' + cls + '">' + match + '</span>';
            });
        },

        closeView: function() {
            this.close();
        },

        onClose: function() {
            this.$el.find(".exceptionModal").off('hidden.bs.modal', this.closeView);
            this.remove();
        }
    });

    return view;
});

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

define(['backbone', 'underscore', 'backgrid'], function(Backbone, _, Backgrid) {

    "use strict";

    Backgrid.cell_initialize_original = Backgrid.Cell.prototype.initialize;
    Backgrid.Cell.prototype.initialize = function() {
        Backgrid.cell_initialize_original.apply(this, arguments);
        var className = this.column.get('className');
        if (className) this.$el.addClass(className);
        if (this.column.attributes.handler) {
            this.column.attributes.handler(this.model, this.$el);
        }
    };

    Backgrid.headerCell_initialize_original = Backgrid.HeaderCell.prototype.initialize;
    Backgrid.HeaderCell.prototype.initialize = function() {
        Backgrid.headerCell_initialize_original.apply(this, arguments);
        var className = this.column.get('headerClassName');
        if (className) this.$el.addClass(className);
    };

    Backgrid.HtmlCell = Backgrid.Cell.extend({
        className: "html-cell",
        render: function() {
            this.$el.empty();
            this.$el.html(this.formatter.fromRaw(this.model.get(this.column.get("name")), this.model));
            this.delegateEvents();
            return this;
        }
    });

    return Backbone;
});

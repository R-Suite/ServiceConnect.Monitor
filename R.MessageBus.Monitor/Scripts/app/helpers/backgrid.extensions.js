define([
    'backbone',
    'underscore',
    'backgrid'
], function(Backbone, _, Backgrid) {

    "use strict";

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

require.config({
    paths: {
        "jquery": 'bower_components/jquery/dist/jquery',
        "underscore": 'bower_components/underscore/underscore',
        "backbone": 'bower_components/backbone/backbone',
        "bootstrap": 'bower_components/bootstrap/dist/js/bootstrap',
        "toastr": "bower_components/toastr/toastr",
        'backbone.stickit': 'bower_components/backbone.stickit/backbone.stickit',
        'backgrid': 'bower_components/backgrid/lib/backgrid',
        'select2': 'bower_components/select2/select2',
        'backbone-pageable': 'bower_components/backbone-pageable/lib/backbone-pageable',
        'backgrid-filter': 'bower_components/backgrid-filter/backgrid-filter',
        'backgrid-paginator': 'bower_components/backgrid-paginator/backgrid-paginator',
        'moment': "bower_components/moment/moment",
        "jasmine": 'bower_components/jasmine/lib/jasmine-core/jasmine',
        "jasmine-html": 'bower_components/jasmine/lib/jasmine-core/jasmine-html',
        "sinon": "bower_components/sinon/lib/sinon",
        "vis": "bower_components/vis/dist/vis"
    },
    shim: {
        'backbone-pageable': {
            deps: ["backbone", "underscore", "jquery"]
        },
        'backgrid-filter': {
            deps: ["backbone", "underscore", "jquery", "backgrid"]
        },
        'backgrid-paginator': {
            deps: ["backbone", "underscore", "jquery", "backgrid"]
        },
        'select2': {
            deps: ["jquery"]
        },
        'backgrid': {
            deps: ["jquery", "backbone", "underscore"],
            exports: "Backgrid"
        },
        "bootstrap": {
            deps: ["jquery"]
        },
        'jquery': {
            deps: [],
            exports: '$'
        },
        'backbone': {
            deps: ["underscore", "jquery"],
            exports: "Backbone"
        },
        'underscore': {
            exports: "_"
        },
        'backbone.stickit': {
            deps: ["backbone", "underscore", "jquery"]
        },
        "jasmine": {
            exports: 'jasmine'
        },
        "jasmine-html": {
            deps: ["jasmine"]
        }
    },
    baseUrl: "..",
    waitSeconds: 200
});

require(['backbone',
         'jquery',
         'jasmine',
         'require',
         "jasmine-html",
         'bootstrap',
         'app/helpers/backbone.extensions',
         'app/helpers/underscore.extensions',
         'app/helpers/backgrid.extensions'
    ],
    function (_, $, jasmine, require) {

    var jasmineEnv = jasmine.getEnv(),
        htmlReporter = new jasmine.HtmlReporter();

    jasmineEnv.addReporter(htmlReporter);

    jasmineEnv.specFilter = function (spec) {
        return htmlReporter.specFilter(spec);
    };

    var specs = [];

    specs.push('tests/spec/router/main.spec');
    specs.push('tests/spec/views/endpoints.spec');
    specs.push('tests/spec/views/services.spec');
    specs.push('tests/spec/views/endpointGraph.spec');
    specs.push('tests/spec/views/navigation.spec');

    $(function(){
        require(specs, function(){
            jasmineEnv.execute();
        });
    });

});
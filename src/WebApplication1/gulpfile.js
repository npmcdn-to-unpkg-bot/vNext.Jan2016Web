/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    gulpFilter = require('gulp-filter');

var paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";
paths.areas = "Areas";

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("clean:areas", function (cb) {
    rimraf(paths.areas, cb);
});

gulp.task('copy:p6.main', function () {
    return gulp.src(['../p6.main/Views/**'])
        .pipe(gulp.dest('Views/'));
});

gulp.task('copy:p6.main:areas', function () {
    return gulp.src(['../p6.main/Areas/**', '!../p6.main/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:Hello.Polymer:areas', function () {
    return gulp.src(['../Hello.Polymer/Areas/**', '!../Hello.Polymer/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:Hello.Polymer:assets', function () {
    return gulp.src(['../Hello.Polymer/assets/**'])
        .pipe(gulp.dest(paths.webroot+'assets/'));
});

gulp.task('copy:p6.PartnerManagement:assets', function () {
    return gulp.src(['../p6.PartnerManagement/assets/**'])
        .pipe(gulp.dest(paths.webroot + 'assets/'));
});



gulp.task('copy:p6.PartnerManagement:areas', function () {
    return gulp.src(['../p6.PartnerManagement/Areas/**', '!../p6.PartnerManagement/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:p6.sports:areas', function () {
    return gulp.src(['../p6.main/Areas/**', '!../p6.main/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:DeveloperAuth', function () {
    return gulp.src(['../DeveloperAuth/Areas/**', '!../DeveloperAuth/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:p6.sports:areas', function () {
    return gulp.src(['../p6.sports/Areas/**', '!../p6.sports/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:p6.animals:areas', function () {
    return gulp.src(['../p6.animals/Areas/**', '!../p6.animals/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:Pingo.Authorization:areas', function () {
    return gulp.src(['../Pingo.Authorization/Areas/**', '!../Pingo.Authorization/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});



gulp.task('watch', [
        'copy:p6.main',
        'copy:p6.main:areas',
        'copy:Hello.Polymer:areas',
        'copy:Hello.Polymer:assets',
        'copy:p6.PartnerManagement:assets',
        'copy:p6.PartnerManagement:areas',
        'copy:p6.sports:areas',
        'copy:p6.animals:areas',
        'copy:Pingo.Authorization:areas',
        'copy:DeveloperAuth'
    ],
    function () {
        gulp.watch(['../p6.main/Views/**'], ['copy:p6.main']);
        gulp.watch(['../p6.main/Areas/**'], ['copy:p6.main:areas']);
        gulp.watch(['../Hello.Polymer/Areas/**'], ['copy:Hello.Polymer:areas']);
        gulp.watch(['../Hello.Polymer/assets/**'], ['copy:Hello.Polymer:assets']);
        gulp.watch(['../p6.PartnerManagement/assets/**'], ['copy:p6.PartnerManagement:assets']);
        gulp.watch(['../p6.PartnerManagement/Areas/**'], ['copy:p6.PartnerManagement:areas']);
        gulp.watch(['../DeveloperAuth/Areas/**'], ['copy:DeveloperAuth']);
        gulp.watch(['../p6.sports/Areas/**'], ['copy:p6.sports:areas']);
        gulp.watch(['../p6.animals/Areas/**'], ['copy:p6.animals:areas']);
        gulp.watch(['../Pingo.Authorization/Areas/**'], ['copy:Pingo.Authorization:areas']);
      
    });

gulp.task("min", ["min:js", "min:css"]);

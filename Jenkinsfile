pipeline {
    agent any
    stages {
        stage('Build') {
    		agent {
        		docker { image 'unityci/editor:ubuntu-2020.3.25f1-webgl-0.16.1' 
				args '-u root:sudo'
//				args '-v /etc/passwd:/etc/passwd:ro'
 	        		alwaysPull false	
        		}
    		}
            steps {
                sh 'unity-editor  -batchmode -manualLicenseFile *.ulf -logfile lic_log.txt | true; cat lic_log.txt'
                sh 'ret=0; unity-editor -quit -batchmode -projectPath ./ -executeMethod WebGLBuilder.build -logFile log.txt || ret=1 | true; cat log.txt; exit $ret'
		sh 'chown -R 117 ./; chgrp -R 122 ./'
		}
        }
	stage('Deploy'){
		steps{
		sh 'docker build -t solarsystem-docker .'
		sh 'docker-compose down'
		sh 'docker-compose up --detach'
		}
	}
    }
  post {
    failure {
	emailext body: 'OOPS: The SolarSystem pipeline failed :(.',
	    subject: 'OOPS: The SolarSystem pipeline failed :(.'
//,	    to: 'doc-g21mscprj18-group@imperial.ac.uk'
// Use default recipients
    }
    always {
	emailext body: 'The SolarSystem works :).',
	    subject: 'The SolarSystem works :).',
	    to: 'jh1521@ic.ac.uk'
    }
  }
}
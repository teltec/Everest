<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" disable-output-escaping="no">

<xsl:function name="functx:trim" as="xs:string" xmlns:functx="http://www.functx.com">
	<xsl:param name="arg" as="xs:string?"/>

	<xsl:value-of select="replace(replace($arg,'\s+$','','m'),'^\s+','','m')"/>
</xsl:function>

<xsl:key name="license-by-name" match="licenses/license" use="@name"/>

<xsl:template match="/global">
<html>
	<head>
		<style>
			.list-entry {
				margin: 12px;
				padding: 12px;
			}
			.list-entry {
				background-color: #FFFBEF;
			}

			.property {
				display: block;
			}
			.property.title {
				font-weight: bold;
			}

			pre {
				font-family: Consolas,Menlo,Monaco,Lucida Console,Liberation Mono,DejaVu Sans Mono,Bitstream Vera Sans Mono,Courier New,monospace,serif;
				margin-bottom: 10px;
				max-height: 600px;
				overflow: auto;
				padding: 5px;
				width: auto;
			}
			code {
				font-family: Consolas,Menlo,Monaco,Lucida Console,Liberation Mono,DejaVu Sans Mono,Bitstream Vera Sans Mono,Courier New,monospace,serif;
			}
		</style>
	</head>
	<body>
		<div class="container">
			<h1>Licenses</h1>
			<div id="dependencies" class="list">
				<h2>Dependencies</h2>
				<xsl:for-each select="dependencies/dependency">
					<div class="dependency list-entry">
						<div class="summary">
							<span class="property title">
								<xsl:value-of select="concat(@title, ' ', @version)" />
							</span>
							<xsl:if test="@project-url != ''">
								<span class="property project-url">
									<a href="{@project-url}"><xsl:value-of select="@project-url"/></a>
								</span>
							</xsl:if>
							<xsl:if test="@alt-project-url != ''">
								<span class="property alt-project-url">
									<a href="{@alt-project-url}"><xsl:value-of select="@alt-project-url"/></a>
								</span>
							</xsl:if>
							<xsl:choose>
								<xsl:when test="@license != ''">
									<span class="property license">
										<xsl:value-of select="@license" />
										<pre>
											<code>
												<xsl:value-of select="text()" />
											</code>
										</pre>
									</span>
								</xsl:when>
								<xsl:when test="@license-ref != ''">
									<span class="property license-ref">
									<xsl:for-each select="key('license-by-name', @license-ref)">
										<xsl:choose>
											<xsl:when test="@official-url != ''">
												<a href="{@official-url}"><xsl:value-of select="@title"/></a>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="@title"/>
											</xsl:otherwise>
										</xsl:choose>
										<pre>
											<code>
												<xsl:value-of select="text()" />
											</code>
										</pre>
									</xsl:for-each>
								</span>
								</xsl:when>
								<xsl:otherwise>Unknown license.</xsl:otherwise>
							</xsl:choose>
						</div>
					</div>
				</xsl:for-each>
			</div>
			<div id="assets" class="list">
				<h2>Assets</h2>
				<xsl:for-each select="assets/asset">
					<div class="asset list-entry">
						<div class="summary" alt="@type">
							<span class="property title">
								<xsl:value-of select="concat(@title, ' ', @version)" />
							</span>
							<xsl:if test="@project-url != ''">
								<span class="property project-url">
									<a href="{@project-url}"><xsl:value-of select="@project-url"/></a>
								</span>
							</xsl:if>
							<xsl:if test="@alt-project-url != ''">
								<span class="property alt-project-url">
									<a href="{@alt-project-url}"><xsl:value-of select="@alt-project-url"/></a>
								</span>
							</xsl:if>
							<xsl:choose>
								<xsl:when test="@license != ''">
									<span class="property license">
										<xsl:value-of select="@license" />
										<pre>
											<code>
												<xsl:value-of select="text()" />
											</code>
										</pre>
									</span>
								</xsl:when>
								<xsl:when test="@license-ref != ''">
									<span class="property license-ref">
									<xsl:for-each select="key('license-by-name', @license-ref)">
										<xsl:choose>
											<xsl:when test="@official-url != ''">
												<a href="{@official-url}"><xsl:value-of select="@title"/></a>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="@title"/>
											</xsl:otherwise>
										</xsl:choose>
										<pre>
											<code>
												<xsl:value-of select="text()" />
											</code>
										</pre>
									</xsl:for-each>
								</span>
								</xsl:when>
								<xsl:otherwise>Unknown license.</xsl:otherwise>
							</xsl:choose>
						</div>
					</div>
				</xsl:for-each>
			</div>
		</div>
</body>
</html>
</xsl:template>

</xsl:stylesheet>
